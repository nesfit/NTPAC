#!/bin/bash
# bash ntpacBenchmarkRemoteSSH.sh h15 /root/ntpac/docker-compose-cassandra.yml /root/ntpac/pcap/m57.pcap 3 4 h15 h16 h17 h18

set -e

if [ "$#" -lt 6 ]; then
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
	ssh root@$MANAGER 'docker pull docker.nesad.fit.vutbr.cz/reassembler:latest && \
		docker pull docker.nesad.fit.vutbr.cz/lighthouse:latest && \
		docker pull docker.nesad.fit.vutbr.cz/loadbalancer:latest && \
		docker pull docker.nesad.fit.vutbr.cz/cassandra:latest && \
		docker pull docker.nesad.fit.vutbr.cz/cassandrainitializer:latest'
}

function deploy_stack() {
	ssh -t root@$MANAGER "docker stack rm ntpac; \
		sleep 7 && \
		docker stack deploy -c $DOCKER_COMPOSE_FILE --with-registry-auth ntpac && \
		docker service update --replicas=$REASSEMBLERS_NUM ntpac_reassembler"
	sleep 2
}

function deploy_and_setup_cassandra_nodes() {
	# Use REASSEMBLERS_NUM-1 Cassandra nodes (one node will be deployed by seed node automatically)
	cassandra_nodes="$(( $REASSEMBLERS_NUM-1 > 0 ? REASSEMBLERS_NUM-1 : 0 ))"
	ssh -t root@$MANAGER "docker service update --replicas=$cassandra_nodes ntpac_cassandra && \
		sleep 1 && \
		docker run --network ntpac_default docker.nesad.fit.vutbr.cz/cassandrainitializer:latest"
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
	ssh root@$1 "docker run --volume $(dirname $PCAP_FILE):/pcap/ --network ntpac_default --env NTPAC_SEED_NODE=lighthouse docker.nesad.fit.vutbr.cz/loadbalancer:latest /pcap/$(basename $PCAP_FILE)" | grep --color='always' -e 'Capture size' -e '^'
}

function remove_stack() {
	ssh root@$MANAGER 'docker stack rm ntpac'
}

function prune_volumes() {
	HOSTS=$(ssh root@$MANAGER "docker node ls --format '{{.Hostname}} {{.Status}}' | grep 'Ready' | cut -d ' ' -f 1")
	for HOST in $HOSTS
	do
		ssh root@$HOST 'docker volume prune --force' &
	done
	wait
}

date
echo

pull_images
echo

deploy_stack
echo

deploy_and_setup_cassandra_nodes
echo

for i in $(seq $EXPERIMENTS_NUM)
do
	echo "Running experiment #$i"
	run_loadbalancers $@
	sleep 5
	echo -e '\n\n\n'
done

remove_stack
prune_volumes

date