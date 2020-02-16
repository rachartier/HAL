Documentation API
=================

Vous pouvez retrouver ici toute la documentation explicative des choix portées sur l'API REST de HAL.


Contenue
--------

- [Schema](#schema)
    - [Base de donnée](#base-de-donnée)
    - [API Gateway](#api-gateway)
- [Description des choix](#description-des-choix)
- [Sécurité](#sécurité)
    - [HTTP](#http-:-les-données-ne-sont-pas-chiffrées)
    - [HTTPS](#https-:-connexions-chiffrées)



Schema
--------

![](files/diag_api.png)

Notre architecture respecte l'architure classique d'une API REST:
* API Gateway avec Ocelot
* API RESTfull avec ASP.NET Core
* Authentification
* Base de données avec MongoDB

Remise en contexte:
Notre sujet est de créer une application permettant de faire remonter des données au serveur grace à l'execution de plugins, faite par le client. Ces données, sous format JSON, sont envoyées sur un serveur, pour permettre un stockage sur ce dernier, ou bien envoyées dans une base de données (MongoDB, InfluxDB). 
Chaque execution des plugins enverra donc ce résultat s'il est différent du précédent dans la base de donnée, avec d'autres informations comme le nom de la machine, la date...

## Base de donnée


La base de données MongoDB est donc alimentée par les clients. Cette base va stocker tous les résultats sous forme de document, ce qui permet de stocker une grande quantitée de données, mais a comme défaut d'être peu performante dans des reqûetes relationnelles.

## API Gateway


L'API Gateway a été réalisée avec Ocelot, qui permet de facilement manager ses liens et ses api.



Description des choix
---------------------

Nous avons choisi de faire une API RESTfull et une API utilisant des websockets.

L'API RESTfull est basique. Elle permet de retrouver tous les résultats de tous les plugins.


## Sécurité



### HTTP : les données ne sont pas chiffrées
HTTP utilisent un protocole simple de transfert hypertexte, il a été créé au début des années 1990 par Tim Berners-Lee.

HTTP est un système sans état, ce qui signifie qu’il permet de créer des connexions à la demande. Une connexion est sollicitée et le navigateur web envoie la demande au serveur qui répond en ouvrant la page.

En tant que protocole de couche d’application, l’unique objectif de HTTP est d’afficher les informations demandées sans se soucier de la façon dont ces informations se déplacent d’un endroit à un autre. Ca signifie que HTTP peut être intercepté et éventuellement détourné, ce qui rend les informations et leurs destinataires vulnérables.

### HTTPS : connexions chiffrées
HTTPS et HTTP, tous deux sont des protocoles de transfert hypertexte qui permettent à des données web d’être affichées lors une requête. HTTPS est légèrement différent, plus avancé et bien plus sécurisé.

Le protocole HTTPS est une extension de HTTP. Le  "S" signifie "Secure" et il fonctionne grâce au protocole TLS (Transport Layer Security), le successeur du protocole SSL (Secure Sockets Layer), la technologie de sécurité standard pour établir une connexion chiffrée entre un serveur web et un navigateur.

En plus de chiffrer les données transmises entre un serveur et le navigateur, le protocole TLS authentifie également le serveur auquel le client se connecte et protège les données transmises de toute altération.

