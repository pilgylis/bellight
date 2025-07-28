echo "**********************************************"
echo "Waiting for startup.."
sleep 20 
echo "done"

echo SETUP.sh time now: `date +"%T" ` 

mongosh 127.0.0.1:27017 -u ${MONGO_INITDB_ROOT_USERNAME} -p ${MONGO_INITDB_ROOT_PASSWORD} << EOF
  rsconf = {
    _id : "rs0",
    members: [
      { _id : 0, host : "localhost:27017", priority:1 }
    ]
  }
  rs.initiate(rsconf);
  
  rs.conf();
EOF