## How to configure HAL-Server with Docker ?

A docker-compose.yml of the container of the server is available at /server/docker-compose.yml.

The container is available in public at [dockerhub.iut-clermont.uca.fr/ui/library/hal\_hal-server\_dotnet3.1](https://dockerhub.iut-clermont.uca.fr/ui/library/hal\_hal-server\_dotnet3.1)

If you start the container as it is, it will be using default value for it's configuration.
The docker-compose.yml make the configuration easier for users

```yaml
version: "3.7"
services:
  hal:
    image: dockerhub.iut-clermont.uca.fr:443/hal_hal-server_dotnet3.1:1.0
    ports:
      - "11000:11000"
    restart: unless-stopped
    volumes:
          - /data/TMP/hal_saves:/data
    environment:
      IP_HAL: "0.0.0.0"
      PORT_HAL: 11000
      MAX_THREADS_HAL: 8
      UPDATE_RATE_HAL: 1000
      SAVE_PATH_HAL: /data
```

Let's have a look to these variables environment:
 - IP\_HAL: Is the varenv which indicate the IP adresse of the server
 - PORT\_HAL: Is the varenv which indicate the port where the server will lookup to for clients and communication __/!\\__ **If you change this variable don't forget to change also the container ports of your docker-compose.yml file** __/!\\__
 - MAX\_THREADS\_HAL: Is the varenv to indicate the use max of thread by HAL server
 - UPDATE\_RATE\_HAL: Is the varenv which indicate the rate of update in millis
 - SAVE\_PATH\_HAL: Is the path where the data will be save into the container which HAL-Server is

## Starting HAL-Server

To start HAL-Server with docker-compose: `docker-compose up`
