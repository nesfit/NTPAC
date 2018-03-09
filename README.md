# NTPAC: Network Traffic Processing & Analysis Cluster
NTPAC is a distributed network forensics tool. 
It is capable of distributing the input network traffic capture (originating from a live network interface or from a PCAP/ng file) among workers in a cluster, which reassemble existing L7 conversations and dissect them using application protocol parsers.


## Deployment of Docker containers
```
1. Join nodes into a Docker Swarm cluster
2. Establish roles of individual nodes in a cluster (single node can have multiple roles):
	Single distributed system seed node: $ docker node update --label-add lighthouse=true NODE
	Reassemblers (worker nodes): $ docker node update --label-add reassembler=true NODE
	Cassandra nodes: $ docker node update --label-add cassandara=true NODE
3. Configure required number of Reassembler and Cassadra nodes by specifiying the number of replicas of reassembler and cassandra services in NTPAC/docker-compose-cassandra.yml.
4. Deploy ntpac stack: $ docker stack deploy -c NTPAC/docker-compose-cassandra.yml --with-registry-auth ntpac
5. Initialize Cassandra database: $ docker run --network ntpac_default docker.nesad.fit.vutbr.cz/cassandrainitializer:latest
6. Run LoadBalancer node with substitued HOST_PCAP_DIRECTORY and PCAP_FILE variables: $ docker run --network ntpac_default --mount type=bind,source=HOST_PCAP_DIRECTORY,target=/pcap docker.nesad.fit.vutbr.cz/loadbalancer:latest -env NTPAC_SEED_NODE=lighthouse docker.nesad.fit.vutbr.cz/loadbalancer:latest /pcap/PCAP_FILE
```