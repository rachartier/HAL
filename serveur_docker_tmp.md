# Serveur Docker Documentation

- [Serveur Docker Documentation](#serveur-docker-documentation)
  - [Docker-Compose du serveur](#docker-compose-du-serveur)
    - [Volumes du serveur](#volumes-du-serveur)
    - [Variables d'environnement](#variables-denvironnement)
  - [Mise à jour du Docker](#mise-à-jour-du-docker)
  - [InfluxDB](#influxdb)
    - [Présentation](#présentation)
    - [Installation InfluxDB](#installation-influxdb)
    - [Volumes InfluxDB](#volumes-influxdb)

## Docker-Compose du serveur

Le docker-compose de l'application actuelle est disponible sur [/docker/server/docker-compose.yml](/docker/server/docker-compose.yml).

Le container du serveur, quant à lui, est disponible en publique sur [dockerhub.iut-clermont.uca.fr/ui/library/hal_hal-server_dotnet3.1](https://dockerhub.iut-clermont.uca.fr/ui/library/hal_hal-server_dotnet3.1).

La configuration du docker-compose est comme suit:

```yaml
version: "3.3"
services:
  hal:
    image: dockerhub.iut-clermont.uca.fr:443/hal_hal-server_dotnet3.1:latest
    ports:
      - "11000:11000"
    restart: unless-stopped
    volumes:
          - /data/TMP/hal/results:/data
          - /data/TMP/hal/plugins:/plugins
    environment:
      IP_HAL: "0.0.0.0"
      PORT_HAL: 11000
      MAX_THREADS_HAL: 8
      UPDATE_RATE_HAL: 1000
      SAVE_PATH_HAL: /data
```

### Volumes du serveur

Côté **hôte**, on a deux volumes: `/data/TMP/hal/results` qui correspond au dossier contenant les résultats des plugins exécuté dans les clients de HAL. Vous pouvez le changer comme vous le voulez, cependant le volume côté **conteneur** ne devra pas changer `/data`.

Le second volume: `/data/TMP/hal/plugins` correspond au dossier contenant les plugins à envoyer aux clients de HAL. Vous pouvez également le changer comme vous le voulez, cependant le volume côté **conteneur** ne devra pas être changer `/plugins`.

### Variables d'environnement

Le docker-compose est composé de variables d'environnement, ces variables servent à modifier la configuration du serveur:

* IP_HAL: La variable d'environnement indiquant l'adresse IP du serveur.
* PORT_HAL: La variable d'environnement qui indique le port ou le serveur iras pointer pour les clients et la communication. **/!\ Si cette variables est changé, ne pas oublier de modifier également le port du conteneur dans le docker-compose.yml (champs 'ports' du docker-compose ci-dessus) /!\\**
* MAX_THREADS_HAL: La variable d'environnement qui indique le nombre max de threads utilisé par le serveur HAL.
* UPDATE_RATE_HAL: La variable d'environnement qui indique la fréquence des updates en millisecondes.
* SAVE_PATH_HAL: C'est le chemin où seront sauvegarder les données récoltés par les plugins dans le container du serveur HAL.

## Mise à jour du Docker

Pour mettre à jour le conteneur, il suffit simplement de se placer dans le même dossier que le docker-compose.yml du serveur est de rentrer cette commande: `docker-compose pull`. Cela mettra à jour automatiquement l'image présente dans le docker-compose.yml du dossier ou vous vous trouver.

Ici il mettra à jour l'image: `dockerhub.iut-clermont.uca.fr:443/hal_hal-server_dotnet3.1:latest`

Et pour lancer le serveur via le docker-compose, il suffit de lancer `docker-compose up` ou `docker-compose up -d` pour un lancement en mode démon.

## InfluxDB

InfluxDB est système de gestion de base de donnée orientée données dite 'timeseries', c'est-à-dire qu'il s'agit de base de donnée contenant des données horodatée. Il possède une gestion haute performance et une API permettant le requêtage de donnée au sein même des bases, et ceux grâce à leur langage de requête nommé InfluxQL.

### Présentation

InfluxDB est l'une des solutions proposé pour stocker les données de recolté par HAL, voir la documentation sur les [stockages](/tree/master/#stockage). Notre documentation fournis un ensemble 'ready to go' permettant de lancer le serveur HAL avec Grafana et InfluxDB connecté.

### Installation InfluxDB

Nous n'avons pas fournis de solution d'installation manuelle pour InfluxDB. Ni même de docker-compose indépendant. Cependant vous pouvez créer votre propre docker-compose indépendant pour votre influxDB si vous souhaitez l'installer sur un serveur distant.

Le service d'influxdb au sein du docker-compose se résume comme suit:

```yaml
  influxdb:
    image: influxdb:latest
    restart: unless-stopped
    ports:
      - "8083:8083"
      - "8086:8086"
      - "8090:8090"
    env_file:
      - 'env.influxdb'
    volumes:
      # Data persistency
      # sudo mkdir -p /srv/docker/influxdb/data
      - /srv/docker/influxdb/data:/var/lib/influxdb
```

Le port 8083 d'influxDB correspond à l'interface web administrateur, vous pouvez bien sur redirigé le port hôte vers un autre port souhaité.
Le port 8086 d'influxDB correspond à l'API exposé en HTTP, c'est ce port qui sera utilisé par Grafana afin d'accèder aux données.

NB: Si le conteneur d'influxDB est situé dans le même docker-compose que celui de Grafana, il n'est pas obligatoire d'exposé l'API afin que tout fonctionne correctement, cependant de cette manière vous ne pourrez pas débugger depuis l'exterieur du réseaux interne du docker-compose.

Pour une installation manuelle de InfluxDB, la [documentation](https://docs.influxdata.com/influxdb/v1.8/introduction/install/) d'influxDB est très bien faite et pourras vous aidez à déployer la solution souhaité.

### Volumes InfluxDB

Vous devrez monter un volume sur votre hôte si vous voulez accèder aux données persisté dans le conteneur.

```yaml
    volumes:
      # Data persistency
      # sudo mkdir -p /srv/docker/influxdb/data
      - /srv/docker/influxdb/data:/var/lib/influxdb
```

Par défaut le chemin vers les données persistés côté hôte est: `/srv/docker/influxdb/data` créé le avec la commande `sudo mkdir -p /srv/docker/influxdb/data`.

Plus d'information sur les spécificités du conteneur influxdb sur [la documentation officielle](https://hub.docker.com/_/influxdb).
