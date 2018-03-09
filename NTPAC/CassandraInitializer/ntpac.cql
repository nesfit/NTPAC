-- DESCRIBE KEYSPACE ntpac;

CREATE KEYSPACE ntpac WITH replication = {'class': 'SimpleStrategy', 'replication_factor': '1'}  AND durable_writes = true;

CREATE TYPE ntpac.dnsanswerentity (
    type tinyint,
    address inet,
    hostname text
);

CREATE TYPE ntpac.dnsqueryentity (
    type tinyint,
    name text
);

CREATE TYPE ntpac.ipendpointentity (
    address inet,
    port int
);

CREATE TYPE ntpac.l7pduentity (
    direction tinyint,
    firstseenticks bigint,
    lastseenticks bigint,
    payload blob
);

CREATE TABLE ntpac.captureentity (
    id uuid PRIMARY KEY,
    firstseen timestamp,
    l7conversationcount int,
    lastseen timestamp,
    processed timestamp,
    uri text
) WITH bloom_filter_fp_chance = 0.01
    AND caching = {'keys': 'ALL', 'rows_per_partition': 'NONE'}
    AND comment = ''
    AND compaction = {'class': 'org.apache.cassandra.db.compaction.SizeTieredCompactionStrategy', 'max_threshold': '32', 'min_threshold': '4'}
    AND compression = {'chunk_length_in_kb': '64', 'class': 'org.apache.cassandra.io.compress.LZ4Compressor'}
    AND crc_check_chance = 1.0
    AND default_time_to_live = 0
    AND gc_grace_seconds = 864000
    AND max_index_interval = 2048
    AND memtable_flush_period_in_ms = 0
    AND min_index_interval = 128
    AND read_repair_chance = 0.0
    AND speculative_retry = '99PERCENTILE';

CREATE TABLE ntpac.httpexportentity (
    id uuid PRIMARY KEY,
    direction tinyint,
    headerfields frozen<map<text, frozen<list<text>>>>,
    l7conversationid uuid,
    method text,
    payload blob,
    payloadincomplete boolean,
    snooper text,
    statuscode smallint,
    statusmessage text,
    timestamp timestamp,
    type tinyint,
    uri text,
    version text
) WITH bloom_filter_fp_chance = 0.01
    AND caching = {'keys': 'ALL', 'rows_per_partition': 'NONE'}
    AND comment = ''
    AND compaction = {'class': 'org.apache.cassandra.db.compaction.SizeTieredCompactionStrategy', 'max_threshold': '32', 'min_threshold': '4'}
    AND compression = {'chunk_length_in_kb': '64', 'class': 'org.apache.cassandra.io.compress.LZ4Compressor'}
    AND crc_check_chance = 1.0
    AND default_time_to_live = 0
    AND gc_grace_seconds = 864000
    AND max_index_interval = 2048
    AND memtable_flush_period_in_ms = 0
    AND min_index_interval = 128
    AND read_repair_chance = 0.0
    AND speculative_retry = '99PERCENTILE';

CREATE TABLE ntpac.genericexportentity (
    id uuid PRIMARY KEY,
    direction tinyint,
    l7conversationid uuid,
    serializeddata text,
    snooper text,
    timestamp timestamp
) WITH bloom_filter_fp_chance = 0.01
    AND caching = {'keys': 'ALL', 'rows_per_partition': 'NONE'}
    AND comment = ''
    AND compaction = {'class': 'org.apache.cassandra.db.compaction.SizeTieredCompactionStrategy', 'max_threshold': '32', 'min_threshold': '4'}
    AND compression = {'chunk_length_in_kb': '64', 'class': 'org.apache.cassandra.io.compress.LZ4Compressor'}
    AND crc_check_chance = 1.0
    AND default_time_to_live = 0
    AND gc_grace_seconds = 864000
    AND max_index_interval = 2048
    AND memtable_flush_period_in_ms = 0
    AND min_index_interval = 128
    AND read_repair_chance = 0.0
    AND speculative_retry = '99PERCENTILE';

CREATE TABLE ntpac.l7conversationpdusshardentity (
    l7conversationid uuid,
    shard int,
    pdus frozen<list<frozen<l7pduentity>>>,
    PRIMARY KEY (l7conversationid, shard)
) WITH CLUSTERING ORDER BY (shard ASC)
    AND bloom_filter_fp_chance = 0.01
    AND caching = {'keys': 'ALL', 'rows_per_partition': 'NONE'}
    AND comment = ''
    AND compaction = {'class': 'org.apache.cassandra.db.compaction.SizeTieredCompactionStrategy', 'max_threshold': '32', 'min_threshold': '4'}
    AND compression = {'chunk_length_in_kb': '64', 'class': 'org.apache.cassandra.io.compress.LZ4Compressor'}
    AND crc_check_chance = 1.0
    AND default_time_to_live = 0
    AND gc_grace_seconds = 864000
    AND max_index_interval = 2048
    AND memtable_flush_period_in_ms = 0
    AND min_index_interval = 128
    AND read_repair_chance = 0.0
    AND speculative_retry = '99PERCENTILE';

CREATE TABLE ntpac.dnsexportentity (
    id uuid PRIMARY KEY,
    answers frozen<list<frozen<dnsanswerentity>>>,
    direction tinyint,
    l7conversationid uuid,
    queries frozen<list<frozen<dnsqueryentity>>>,
    snooper text,
    timestamp timestamp,
    transactionid int,
    type tinyint
) WITH bloom_filter_fp_chance = 0.01
    AND caching = {'keys': 'ALL', 'rows_per_partition': 'NONE'}
    AND comment = ''
    AND compaction = {'class': 'org.apache.cassandra.db.compaction.SizeTieredCompactionStrategy', 'max_threshold': '32', 'min_threshold': '4'}
    AND compression = {'chunk_length_in_kb': '64', 'class': 'org.apache.cassandra.io.compress.LZ4Compressor'}
    AND crc_check_chance = 1.0
    AND default_time_to_live = 0
    AND gc_grace_seconds = 864000
    AND max_index_interval = 2048
    AND memtable_flush_period_in_ms = 0
    AND min_index_interval = 128
    AND read_repair_chance = 0.0
    AND speculative_retry = '99PERCENTILE';

CREATE TABLE ntpac.l7conversationentity (
    id uuid PRIMARY KEY,
    captureid uuid,
    destinationendpoint ipendpointentity,
    firstseen timestamp,
    lastseen timestamp,
    pducount bigint,
    protocoltype int,
    sourceendpoint ipendpointentity
) WITH bloom_filter_fp_chance = 0.01
    AND caching = {'keys': 'ALL', 'rows_per_partition': 'NONE'}
    AND comment = ''
    AND compaction = {'class': 'org.apache.cassandra.db.compaction.SizeTieredCompactionStrategy', 'max_threshold': '32', 'min_threshold': '4'}
    AND compression = {'chunk_length_in_kb': '64', 'class': 'org.apache.cassandra.io.compress.LZ4Compressor'}
    AND crc_check_chance = 1.0
    AND default_time_to_live = 0
    AND gc_grace_seconds = 864000
    AND max_index_interval = 2048
    AND memtable_flush_period_in_ms = 0
    AND min_index_interval = 128
    AND read_repair_chance = 0.0
    AND speculative_retry = '99PERCENTILE';
CREATE INDEX l7conversationentity_captureid_idx ON ntpac.l7conversationentity (captureid);
