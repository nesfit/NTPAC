#!/bin/bash
# ./ntpacBenchmarkRemoteGcp.sh swarm-mngr ../docker-compose-cassandra.yml /home/vilco/pcap/m57.pcap 3 2 lb01

set -e
unset GREP_OPTIONS

if [ "$#" -lt 5 ]; then
    echo "./ntpacBenchmark.sh MANAGER DOCKER_COMPOSE_FILE PCAP_FILE EXPERIMENTS_NUM REASSEMBLERS_NUM LB_HOST [LB_HOST ...]"
    exit 1
fi

MANAGER=$1
DOCKER_COMPOSE_FILE=$2
PCAP_FILE=$3
EXPERIMENTS_NUM=$4
REASSEMBLERS_NUM=$5

shift; shift; shift; shift; shift

function pull_images() {
	eval $(docker-machine env $MANAGER)
	docker pull docker.nesad.fit.vutbr.cz/reassembler:latest
	docker pull docker.nesad.fit.vutbr.cz/lighthouse:latest
	docker pull docker.nesad.fit.vutbr.cz/loadbalancer:latest
	docker pull docker.nesad.fit.vutbr.cz/cassandra:latest
	docker pull docker.nesad.fit.vutbr.cz/cassandrainitializer:latest
}

function deploy_stack() {
	eval $(docker-machine env $MANAGER)
	docker stack deploy -c $DOCKER_COMPOSE_FILE --with-registry-auth ntpac
	docker service update --replicas=$REASSEMBLERS_NUM ntpac_reassembler
	sleep 2
}

function deploy_and_setup_cassandra_nodes() {
	# Use REASSEMBLERS_NUM-1 Cassandra nodes (one node will be deployed by seed node automatically)
	cassandra_nodes="$(( $REASSEMBLERS_NUM-1 > 0 ? REASSEMBLERS_NUM-1 : 0 ))"
	eval $(docker-machine env $MANAGER)
	docker service update --replicas=$cassandra_nodes ntpac_cassandra 
	sleep 1
	docker run --network ntpac_default docker.nesad.fit.vutbr.cz/cassandrainitializer:latest
	sleep 1
}

function run_loadbalancers() {
	for LB_HOST in $@
	do
		sleep 2
		echo "Running LB at $LB_HOST"
		run_loadbalancer $LB_HOST &
	done
	wait
}

function run_loadbalancer() {
	eval $(docker-machine env $1)
	docker pull docker.nesad.fit.vutbr.cz/loadbalancer:latest
	docker run --volume $(dirname $PCAP_FILE):/pcap/ --network ntpac_default --env NTPAC_SEED_NODE=lighthouse docker.nesad.fit.vutbr.cz/loadbalancer:latest /pcap/$(basename $PCAP_FILE) | ggrep --color='always' -e '^' -e 'Capture size'
}

function remove_stack() {
	eval $(docker-machine env $MANAGER)
	docker stack rm ntpac
}

function prune_volumes() {
	eval $(docker-machine env $MANAGER)
	HOSTS=$(docker node ls --format '{{.Hostname}} {{.Status}}' | grep 'Ready' | cut -d ' ' -f 1)
	for HOST in $HOSTS
	do
		(eval $(docker-machine env $HOST) && docker volume prune --force) &
	done
	wait
}

date
echo

remove_stack
prune_volumes
sleep 7

deploy_stack
echo

deploy_and_setup_cassandra_nodes
echo

for i in $(seq $EXPERIMENTS_NUM)
do
	echo "Running experiment #$i"
	run_loadbalancers "$@"
	sleep 5
	echo -e '\n\n\n'
done

remove_stack
sleep 10
prune_volumes

date
