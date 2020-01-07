docker run --name halmongo -d -p 27017:27017 -p 28017:28017 -e MONGODB_USER="hal" -e MONGODB_DATABASE="haldb" -e MONGODB_PASS="hal" mongo
