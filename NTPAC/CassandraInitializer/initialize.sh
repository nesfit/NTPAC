#!/bin/sh

NTPAC_CQL_FILE='ntpac.cql'
NTPAC_KEYSPACE='ntpac'
CASSANDRA_SEED_NODE=${CASSANDRA_SEED_NODE:-tasks.cassandra-seed}

wait_for_cassandra_seed_node () {
    while : ;
    do
        echo | cqlsh "$CASSANDRA_SEED_NODE" >/dev/null 2>&1 && break
        sleep 1
    done
}

get_number_of_cassandra_nodes () {
    cassandra_seed_nodes=$(getent hosts tasks.cassandra-seed | wc -l)
    cassandra_nodes=$(getent hosts tasks.cassandra | wc -l)
    expr "$cassandra_seed_nodes" + "$cassandra_nodes"
}

wait_for_all_cassandra_nodes () {
    expected_nodes=$1
    while : ;
    do
        up_nodes=$(nodetool -h $CASSANDRA_SEED_NODE status | grep '^UN' | wc -l)
        [ "$up_nodes" -eq "$expected_nodes" ] && break
        echo -n "$up_nodes/$expected_nodes UP" '\r'
        sleep 1
    done
    echo 'All Cassandra nodes UP'
}

if ! getent hosts $CASSANDRA_SEED_NODE >/dev/null
then
    echo "Failed to resolve Cassandra seed node ($CASSANDRA_SEED_NODE). Is it up?"
    exit 1
fi

echo 'Waiting for seed node ...'
wait_for_cassandra_seed_node

echo 'Dropping keyspace ...'
echo "DROP KEYSPACE IF EXISTS $NTPAC_KEYSPACE;" | cqlsh $CASSANDRA_SEED_NODE

echo 'Waiting for all nodes to became up ...'
wait_for_all_cassandra_nodes $(get_number_of_cassandra_nodes)

echo 'Creating keyspace and schema ...'
cat "$NTPAC_CQL_FILE" | cqlsh $CASSANDRA_SEED_NODE

echo 'Repairing cluster ...'
nodetool -h $CASSANDRA_SEED_NODE repair >/dev/null
