#!/bin/bash
cp /temp/mongodb.pem /data/mongodb.pem
cp /temp/mongodb.keyfile /data/mongodb.keyfile
chmod 400 /data/mongodb.pem
chown 999:999 /data/mongodb.pem
chmod 400 /data/mongodb.keyfile
chown 999:999 /data/mongodb.keyfile
exec /data/setup.sh & exec docker-entrypoint.sh --keyFile /data/mongodb.keyfile --replSet rs0 --bind_ip_all