FROM cassandra:3.11.3 as base
WORKDIR /ntpac
COPY initialize.sh .
COPY ntpac.cql .
ENTRYPOINT ["/ntpac/initialize.sh"]
