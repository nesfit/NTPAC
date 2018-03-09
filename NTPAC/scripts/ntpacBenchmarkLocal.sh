#!/bin/bash
# ./ntpacBenchmark.sh /home/letavay/ntpac/docker-compose.yml /mnt/data2/letavay/pcap/m57.pcap 3

set -e

if [ "$#" -ne 3 ]; then
    echo "./ntpacBenchmark.sh DOCKER_COMPOSE_FILE PCAP_FILE EXPERIMENTS"
    exit 1
fi

DOCKER_COMPOSE_FILE=$1
PCAP_FILE=$2
EXPERIMENTS=$3

function run_loadbalancer() {
	docker run --volume $(dirname $1):/pcap/ --network ntpac_default --env NTPAC_SEED_NODE=lighthouse -it docker.nesad.fit.vutbr.cz/loadbalancer:latest /pcap/$(basename $1)
}

date
echo

docker pull docker.nesad.fit.vutbr.cz/reassembler:latest
docker pull docker.nesad.fit.vutbr.cz/lighthouse:latest
docker pull docker.nesad.fit.vutbr.cz/loadbalancer:latest
echo

docker stack rm ntpac || true
sleep 7
docker stack deploy -c $DOCKER_COMPOSE_FILE --with-registry-auth ntpac
sleep 10
echo

for i in $(seq $EXPERIMENTS)
do
	run_loadbalancer $PCAP_FILE
	sleep 5
	echo -e '\n\n'
done

docker stack rm ntpac

date