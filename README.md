# HAL


[![pipeline status](https://gitlab.iut-clermont.uca.fr/rachartier/hal/badges/master/pipeline.svg)](https://gitlab.iut-clermont.uca.fr/rachartier/hal/commits/master)


***Retrouvez tout les éléments de documentation dans la partie [documentation](documents/) du projet***

Contents
--------

- [Intro](#intro)
- [Installation](#installation)
- [Server](#server)
- [QuickStart](#quickstart)
- [Stockage](#stockage)
- [Ajouter une extension personnalisée](#ajouter-une-extension-personnalisée)
- [Docker](#docker)

Intro
------------

HAL est un projet de supervision destiné à récupérer différents donnés d'un parc informatique et à les envoyer sur un serveur, dans le but d'agréger les données en vue des traitres.
Il utilise un système de plugins, qui sont chargés automatiquement au démarrage du client. Plusieurs langages pour écrire les plugins sont supportés:
* C/C++/C#/Go (.dll / .so)
* Go Script (.go)
* Python (.py)
* Ruby (.rb)
* Shell (.sh)
* Powershell (.ps1)

Et d'autres peuvent être ajoutés manuellement si besoin.

Les plugins sont déposés dans le dossier "plugins", qui est un dossier spécial scanné permettant de charger automatiquement tous les plugins qui s'y trouvent. 

HAL est destiné à tout utilisateur voulant superviser les ordinateurs sur un réseau.

Installation
------------

Pour le bon fonctionnement de HAL, il faut impérativement avoir 

*  dotnet core 3.x, (serv/client)
*  python3.x (client, si vous souhaitez utiliser des plugins en python)
*  ruby (client, si vous souhaitez utiliser des plugins en ruby)
*  golang (client, si vous souhaitez utiliser des plugins en go)

Ou des erreurs peuvent subvenirs pendant l'éxecution des plugins.


Server
-------

Le serveur permet de distribuer les plugins automatiquement à tous les clients. Si un plugin est ajouté ou modifié, de même pour le fichier de configuration, alors le serveur enverra les modifications aux clients.
Pour configurer le serveur, il faut modifier le fichier config/config.json:

*  ip (string): l'ip sur laquelle comuniquera le serveur
*  port (int): le port d'écoute (par défaut le port 11000)
*  max\_threads (int): optionnel, permet de gérer le nombre de threads utilisés par le serveur, par défault, il en utilise le nombre maximum. A noter qu'en dessous de 2 threads et avec beaucoup de clients (> 500), des lags peuvent apparaitre  
*  update\_rate (int): la frequence d'actualisation en ms du serveur pour vérifier si des plugins ont éte ajoutés ou modifiés
* save\_path (string): chemin de sauvegardes des resultats des plugins quand l'option est selectionnée

### Fichier de configuration

2 types de fichiers de configurations sont présents:
*  config\_local.json
*  config\_global.json

Le premier, config_local.json, sert uniquement à rajouter des configurations de plugin. Il est déposé en local dans le dossier "config" sur les *clients*, et ne sera en aucun cas supprimé ou modifié par le serveur.
Le second, quand à lui, sert à modifier tous ce qui est possible dans HAL. Tout est détaillés plus bas dans la documentation. Ce fichier sera distribué à tous les clients.

Il sera impératif de rédiger son propre config\_global.json, et il faudra le placer dans le dossier "plugins" du serveur.
Pour ajouter un plugin, il suffit uniquement de le deposer dans le dossier "plugins" du serveur. 

Pour régler la connection au serveur d'un client, il faut ajouter dans le fichier de configuration local:

```json
"server": {
    "ip": "<ip du serveur>",
    "port": <port du serveur>
}
```

QuickStart
---------------------

### Utilisation du serveur avec Docker

1. Télécharger la dernière image docker du serveur [dockerhub.iut-clermont.uca.fr](https://dockerhub.iut-clermont.uca.fr/ui/library/hal_hal-server_dotnet3.1)

`docker pull dockerhub.iut-clermont.uca.fr:443/hal_hal-server_dotnet3.1:latest`

2. Télécharger le [docker-compose](/dev/docker/server) par défaut du serveur

3. Lancer le docker-compose
`docker-compose up \chemin du docker-compose du serveur\`
NB: Si vous n'avez pas installé docker-compose suivez ce lien [docs.docker.com](https://docs.docker.com/compose/install/)

Pour plus d'information concernant le docker-compose et sa configuration, référez vous au [README](dev/docker) du dossier docker du projet.

Une fois le serveur lancé vous pouvez commencer à utilisé le serveur HAL. Par défaut le port de connexion de HAL est le port 11000 et l'IP par défaut et l'IP local.

Vous pouvez maintenant commencer à écrire vos propres plugins afin de les transférer aux clients.

### Rédaction d'un plugin

Les informations retournées par le plugin doivent être sous un format JSON.
Pour une efficacité optimal, il convient de normaliser les soties de vos plugins. C'est à dire, respecté une convention de nommage définit selon vos principes.

Chaque plugin doit être mit dans le dossier "plugins" du serveur, qui se chargera de les transmettres aux clients. De plus, une entrée doit être écrite dans le fichier config.json pour avoir les informations nécéssaire au bon déroulement du plugin. 

#### Rédaction d'un plugin avec un langage de script

Toutes les sorties sur l'entrée standard seront sauvegardées par le client.

Un exemple de rédaction d'un plugin (info\_machines.py) pour récupérer diverses informations de la machine en python:

``` python
import platform
import json

osInfo = {
	"machine":platform.machine(),
	"version":platform.version(),
	"platform":platform.platform(),
	"system":platform.system(),
	"processor":platform.processor()
}

print(json.dumps(osInfo));
```

Le fichier de configuratin (global) peut être modifié pour accepter un intepréteur différent de celui par défaut.

```json
{
	"interpreter": {
		"windows": {
			"python": "<chemin vers python>\python.exe"
		},
		"linux": {
			"python": "/usr/bin/python3",
		},
	},

	"plugins": {
	}
}
```

2 familles de système d'exploitation sont disponibles:
	
* windows (Windows 7, 8, 10...)
* linux (toutes distribution utilisant le noyaux linux)

Il existe aussi la possibilité de configurer une variable d'environnement (en fonction de votre OS), contenant alors le chemin vers l'intepréteur. Il n'est donc pas obligé de modifier le fichier de configuration avec cette méthode.
La variable d'environnement doit avoir comme clé le nom en majuscule (ex: PYTHON, RUBY, POWERSHELL...) et comme valeur le chemin vers l'intepréteur

Exemple sous Linux:

```
PYTHON=/usr/bin/python3
RUBY=/usr/bin/ruby
```

Pour linux, des intepréteurs par défaut sont déjà configurés, il n'est pas alors obligé de les spécifier pour les types de scripts supportés, bien que cela soit très recommandés.

Par la suite, il faut ajouter la configuration du plugin portant le nom et extension (nom.extension) dans la branche "plugins":

* activated (booléen): permet de savoir si le plugin est actif ou non, par conséquent si il doit être executé
* heartbeat (double): 1 execution par heartbeat. Correspond à l'execution périodique d'un plugin où le hearthbeat défini la période.
    * plus le heartbeat est petit, plus nombreux seront les executions par minute, et donc par conséquent une plus grande charge système.
* os (array de string): 
    * optionnel, permet de lancer le plugin uniquement sur un ou plusieurs famille de système d'exploitation
    * si rien n'est spécifié, le plugin sera disponible sur toutes les familles
    * si l'OS cible est linux, une autre option peut être spécifiée:
* admin_rights (booléen): si le script doit être exécuté en administrateur (sudo), si non spécifié, alors false par défaut


Exemple de configuration:

```json
{
	"interpreter": {
		"windows": {
			"python": "<chemin vers python>\python.exe"
		},
		"linux": {
			"python": "/usr/bin/python3",
		}
	},

	"plugins": {
		"info_machine.py": {
			"activated": "true",
			"heartbeat": "0.5",
			"os": ["windows", "linux"]
		}
	}
}
```

Ce dernier sera alors activé, aura une execution toutes les demie-minutes et sera lancé uniquement sur les plateformes windows et linux.

##### Ajout du mode administrateur

Un plugin peut potentiellement avoir besoin d'être éxecuté avec un utilisateur différent que celui actuellement sur la machine.

Pour cela, il faut rajouter mettre "admin\_rights" à "true" et spécifier un utilisateur: "admin_username" avec le nom d'utilisateur.

Exemple:

```json 
"plugins": {
	"script.sh": {
		"activated": "true",
		"heartbeat": "0.5",
		"admin_rights": "true",
		"admin_username": "user",
		"os": ["linux"]
	}
}
```

Le plugin sera alors lancé automatiquement avec la commande suivant: `sudo -u user -s <shell specifié dans interpreteur> -c script.sh`

##### Mode "differencial"

Ce mode sert à qu'un résultat de plugin soit pas à nouveau sauvegardé si sa valeur n'a pas changé depuis la dernière éxecution.

Exemple:
A la première execution du plugin, le json obtenu est:
```json
    {"kernel_version": "debian 4.19"}
```

Le résultat est alors stocké. Mais si aux résultats suivant le kernel version est toujours "debian 4.19", alors ceux si seront ignorés.

Utilisation:

differencial_all (bool): si à True, alors tous les champs du json seront vérifiés, si un a changé, alors le résultat sera sauvegardé.
differencial (string array): permet de spécifier les champs à observer.

Exemple:
```json
"plugins": {
	"script.py": {
		"activated": "true",
		"heartbeat": "1",
		"differencial_all": true
		
	}
}
```

```json
"plugins": {
	"script.py": {
		"activated": "true",
		"heartbeat": "1",
		"differencial": [
		    "champs1",
		    "champs2"
		]
		
	}
}
```


#### Rédaction d'un plugin AssemblyDLL, classique DLL, Shared object

##### Exemple en C/C++ (classique DLL, Shared Object)

Un point d'entrée est obligatoire pour l'execution du plugin. Le retour de ce point d'entrée sera alors sauvegardé par le client. Il doit impérativement être en JSON.

``` c
char* run() {

}
```

Exemple d'un code en C (ip\_infos.c) uniquement disponible sur linux (ifaddrs.h) permettant de récupérer le nom de la carte réseau, son addresse et les données.
``` c
#include <ifaddrs.h>
#include <stdio.h>
#include <stdlib.h>

char *run(void) {
	struct ifaddrs* id;
	char* ret = malloc(1024);

	getifaddrs(&id);

	sprintf(ret, "{ \"name\": \"%s\", \"addr\": %d, \"data\": %d}", id->ifa_name, id->ifa_addr, id->ifa_data);
	
	return ret;
}
```

Il faut impérativement que la variable de retour soit allouée en mémoire (via malloc, calloc...)  
Des fonctions peuvent bien sûr être faites pour clarifier le code.

Compilation en .so sous linux:
	`gcc -Wall -Wextra -fPIC -shared ip_infos.c -o ip_infos.so`

Compilation en .dll sous windows (si plugin compatible) avec utilisation de [MinGW](http://www.mingw.org/)
	`gcc -Wall -Wextra -fPIC -shared fichier.c -o fichier.dll`

puis copier ip_infos.so dans le dossier plugins et rajouter une entrée dans config.json:

```json
 ...
	"plugins": {
	    ...
		"ip\_infos.c.dll": {
			"activated": "true",
			"heartbeat": "0.5",
			"os": ["windows"]
		},
	    "ip_infos.c.so": {
			"activated": "true",
			"heartbeat": "0.5",
			"os": ["linux"]
		}
		...
	}
 ...
```

##### Exemple en C# (AssemblyDLL)

Un point d'entrée est aussi obligatoire pour l'execution du plugin. Le retour du point d'entrée sera alors sauvegardé par le client. Il doit impérativement être en JSON.

```cs
namespace Plugin 
{
	public class NomPlugin 
	{
	    public string Run() 
	    {

	    }
	}
}
```

Création d'un projet dotnet:

`dotnet new classlib -n plugin`

Modifier Class1.cs pour écrire son plugin
Exemple d'un plugin pour récupérer le nom de la machine:

```cs
using System;

namespace plugin
{
    public class MachineName 
    {
	    public string Run() 
	    {
		    return $"{{\"machinename\": {Environment.MachineName}}}";
	    }
    }
}
```

Puis build la librairie:

`dotnet build`

Finalement, copier et renommer si besoin plugin.dll et le mettre ensuite dans le dossier plugins et ajouter une entrée dans config.json:

```json
 ...
	"plugins": {
	    ...
		"plugin.dll": {
			"activated": "true",
			"heartbeat": "0.5"
		}
		...
	}
 ...
```

### Exemple en GO

Go est intéprété en tant que langage de script dans HAL. De ce fait, il doit respecter les contraintes des autres langages de script, c'est à dire defaire sortir le JSON sur la sortie standard.

Exemple d'un plugin 

```go
package main

import (
    "fmt"
)

func main() {
    fmt.Print("{\"test\": \"reussi\"}")
}

```

Il est aussi possible de faire du Go compilé (sous forme de DLL). L'avantage étant que le client n'est pas obligé de posseder golang. 

```go
package main

import "C"

func main() {
}

//export run
func run() *C.char {
	return C.CString("{\"test\": \"reussi\"}")
}
```

Il est imépratif d'avoir le commentaire "export run" pour que cela fonctionne. De plus, il faut aussi obligatoirement mettre *C.char comme type de retour.

Compilation: `go build -o testplugin.dll -buildmode=c-shared`


### Vérifier que les plugins ont une sortie JSON correcte

Un outil a été crée dans le but de vérifier si une collection de plugins renvoient un json valide.
Pour ça, il faut se rendre dans "plugins_checker" et modifier le fichier "config.json" pour mettre un ou plusieurs chemins de là où se trouve les plugins et le fichier de configuration de ces derniers.

Il faut impérativement suivre ce schéma:
* dossier
    * config.json
    * plugins
        * nomplugin1...
        * nomplugin2...
        * nomplugin3...
            ...

Example:
Un dossier "test1" contient:
* config.json
* plugins/
    * cpu_temperature.py
    * kernel_version.sh
        

et un autre dossier, test2, contient quand à lui:
* config.json
* plugins/
    * connected_user.sh
    * os_informations.rb

Il faut alors tout simplement modifier le fichier "config.json" de plugins_checker comme suit:

```json
{
  "paths": [
    "chemin/vers/test1",
    "chemin/vers/test2"
  ]
}
```
Puis finalement executer le programme.


Démonstration avec les plugins dans client et client/examples:

<a href="https://asciinema.org/a/iidUR3zKPFVWJvNndhzm8RHMV"><img src="https://asciinema.org/a/iidUR3zKPFVWJvNndhzm8RHMV.png" width="836"/></a>

L'erreur: "Error reading JToken from JsonReader. Path '', line 0, position 0."
peut signifier plusieurs choses:
*  un intépreteur par défaut n'est pas installer, il faut vérifier si ruby, python3 et bash sont bien installés sur la machine
*  le retour n'est pas du format json

Ajouter une extension personnalisée
---------------------

Pour ajouter une extension personnalisée, il suffit d'ajouter "custom_extensions" dans le fichier de configuration.
Par exemple, pour ajouter une extension de PHP (.php):

```json
{
  "custom_extensions": {
  	".php": "php"
  }
  "interpreter": {
	"linux": {
	  "php": "path/to/php"
	}
	...
  },

  "plugins": {
	...
  }
}
```

Ne surtout pas oublier de rajouter un intépréteur, car aucun n'a été défini par défaut, que ce soit via config.json ou les variables d'environnements.

Stockage
--------

Actuellement, 4 formes de stockage existe:
* Format texte, sortie sur la console
* Sauvegarde en locale sur le client
* Sauvegarde en base de donnée (MangoDB)
* Sauvegarde en base de donnée orientée séries chronologiques (InfluxDB)
* 
Le stockage sert a sauvegarder le résultat de chaque plugin pour pouvoir ensuite manipuler ces données.

Pour séléctionner le stockage voulu, il faut créer un attribut dans le fichier "config.json":
```json
{
	"storage": [
	    "<NOM_DU_STOCKAGE>"
	 ],

	...
	"plugins": {
		...
	}
}
```

NOM_DU_STOCKAGE peut être:
* "text" (sauvegarde sur la sortie console)
* "local" (sauvegarde sur la machine client)
* "mangodb" (sauvegarde sur une base mongodb)
* "influxdb" (sauvegarde sur une base influxdb)
* "serveur" (sauvegarde sur le serveur avec des dossiers et fichiers json)


Attention, si vous utilisez "mangodb" ou tout autre base de donnée, il faut alors généralement spécifier une connection string:
```json
{
	"storage": [
	    "mangodb"
	],
	"database": {
		"connectionString": [
		    "mongodb://mongodb0.example.com:27017/admin"
		]
	},
	...
}
```

Il est possible de mettre autant de nom de stockage qui nécessaire. Pour lier les bonnes connectionString aux base de données, il faut alors les mettres dans l'ordre, exemple:

```json
{
	"storage": [
	    "<db1>",
	    "<db2>",
	    "server",
	    "<db3>"
	],
	"database": {
		"connectionString": [
		    "<connection string de db1>",
		    "<connection string de db2>",
		    "<connection string de db3>"
		 ]
	},
	...
}
```

Pour rajouter un stockage, il faut alors modifier le code source.
Il faut impérativement créer une classe héritant de IStoragePlugin, et par la suite créer ce dont vous avez besoin.

Ensuite, il faut modifier le fichier: client/factory/StorageFactory.cs pour y rajouter votre stockage personnalisé. Toutes les informations de comment procéder sont mit en commentaires dans ce fichier.

Docker

--------

Le serveur peut être utilisé via un conteneur Docker disponible sur [dockerhub.iut-clermont.uca.fr/ui/library/hal\_hal-server\_dotnet3.1](https://dockerhub.iut-clermont.uca.fr/ui/library/hal_hal-server_dotnet3.1).
Pour plus d'informations se référer au [README](dev/docker) disponible dans le dossier dev/docker/

