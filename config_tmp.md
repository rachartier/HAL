- [Configuration](#configuration)
	- [Comprendre `config.json` et `config_local.json`](#comprendre-configjson-et-config_localjson)
	- [Configurer le serveur](#configurer-le-serveur)
		- [Changement d'ip et de port](#changement-dip-et-de-port)
	- [Configurer le client](#configurer-le-client)
		- [Attributs](#attributs)
		- [Mode administrateur](#mode-administrateur)
		- [Mode differencial](#mode-differencial)
- [Configuration des clients par le serveur](#configuration-des-clients-par-le-serveur)
- [Configurer les sauvegardes des résultats](#configurer-les-sauvegardes-des-résultats)
- [Rédaction plugins](#rédaction-plugins)
	- [Difference entre AssemblyDLL, DLL classique, shared object et script](#difference-entre-assemblydll-dll-classique-shared-object-et-script)
	- [Rédaction de plugins via langages supportés par défaut](#rédaction-de-plugins-via-langages-supportés-par-défaut)
		- [Exemple en C/C++ (DLL classique, Shared Object)](#exemple-en-cc-dll-classique-shared-object)
		- [Exemple en C# (AssemblyDLL)](#exemple-en-c-assemblydll)
		- [Exemple en GO (Script / DLL classique)](#exemple-en-go-script--dll-classique)
		- [Exemple en Python (Script)](#exemple-en-python-script)
	- [Via langage non supporté par défaut (ET scripts Windows)](#vialangagenonsupportépardéfaut-et-scripts-windows)
		- [Ajout d'un interpreteur via fichier de configuration](#ajoutduninterpreteur-via-fichier-de-configuration)
		- [Ajout d'une extension de plugin personnalisée](#ajout-dune-extension-de-plugin-personnalisée)
		- [Ajout d'in interpreteur via variables d'environnements](#ajout-din-interpreteur-via-variables-denvironnements)
- [Vérification des sorties des plugins](#vérificationdessortiesdesplugins)
	- [Présentation du plugins_checker](#présentationduplugins_checker)
- [Configurer les sauvegardes des résultats](#configurer-les-sauvegardes-des-résultats-1)

Configuration
-------------
### Comprendre `config.json` et `config_local.json` 

2 types de fichiers de configurations sont présents:
*  config\_local.json
*  config\_global.json

`config_local.json`: sert uniquement à rajouter des configurations de plugin. Il est déposé en local dans le dossier "config" sur les *clients*, et ne sera en aucun cas supprimé ou modifié par le serveur.

`config.json`: sert à modifier tout ce qui est possible dans HAL. Tout est détaillés plus bas dans la documentation. Ce fichier sera distribué et mit à jour à tout les clients via le serveur.

Il sera impératif de rédiger son propre `config.json`, et de l'ajouter dans le dossier "plugins" du serveur.

### Configurer le serveur
#### Changement d'ip et de port

Pour régler la connection au serveur d'un client, il faut ajouter dans le fichier `config_local.json`, présent sur les clients:

```json
{
	"server": {
		"ip": "<ip du serveur>",
		"port": <port du serveur>
	}
}
```

- `ip`: l'ip d'un des serveurs HAL
- `port`: port d'un des serveurs HAL, par défaut `11000`

**Exemple d'un `config.json` (\<dossier du serveur\>/plugins/config.json)**:

```json
{
  "storage": [
      "server"
  ],

  "plugins": {
    "upgrades_available.sh": {
      "activated": "true",
      "os": [
        "linux"
      ],
      "heartbeat": 0.010
    },
    "kernel_version.py": {
      "activated": "true",
      "heartbeat": 0.010,
      "differencial_all": true
    },
    "connected_user.py": {
      "activated": "true",
      "heartbeat": 0.010,
      "differencial": [
        "connected_user"
      ]
    }
  }
}
```

**Exemple d'un `config_local.json` (\<dossier du client\>/config/config_local.json)**:
```json 
{
  "server": {
    "ip": "XX.XX.XX.XX",
    "port": 11000
  }
}
```

### Configurer le client

La configuration du client du client peut se faire dans le fichier `config.json`, présent dans le dossier "plugins" du serveur, ce qui permettra de facilement ajouter, supprimer, modifier la configuration de tout les clients sans problèmes.

Si besoin, un client peut avoir d'autres plugins en plus, pour cela, il faut les ajouter dans `config_local.json`.


#### Attributs

| Nom       | Optionnel |   Type   | Signification |
| --------- | :-------: | :------: | ------------------------------------------------------------------------------------------- |
| activated |           |   bool   | le plugin est activé si `"true"`, desactivé si `"false"`                                             |
| heartbeat |           |   uint   | fréquence d'éxecution du plugin, 1 heartbeat = 1 execution par minute, 2 = 2 par minute ... |
| os        |     X     | string[] | sur quel système d'exploitation sera executé le plugin. Valeurs possible: `linux / windows`. Si non spécifié, le plugin sera executé sur tout les OS. |
| admin_rights | X | bool | execute le plugin en mode administrateur si `"true"`, mode utilisateur si `"false"` |
| admin_username | X | string | si admin_rights est activé, lance le plugin en mode administrateur avec l'utilisateur spécifié |
|differencial_all| X | bool | vérifie si le résultat json du plugin est différent de l'ancien pour TOUT ses attributs. Si tout les résultats des attributs sont identiques, alors le retour est ignoré  |
| differencial | X | string[] | vérifie si le résultat json du plugin est différent de l'ancien pour les attributs . Si tout les résultats des attributs explicités sont identiques, alors le retour est ignoré  |

**Exemple**:

```json 
{
  "plugins": {
    "upgrades_available.sh": {
      "activated": "true",
      "os": [
        "linux"
      ],
      "heartbeat": 0.010
    },
    "kernel_version.py": {
      "activated": "true",
      "heartbeat": 0.010,
      "differencial_all": true
    },
    "connected_user.py": {
      "activated": "true",
      "heartbeat": 0.010,
      "differencial": [
        "connected_user"
      ]
    },
	"do_upgrades.sh": {
      "activated": "true",
      "os": [
        "linux"
      ],
      "heartbeat": 0.001,
	  "admin_rights": "true",
	  "admin_username": "usr_do_upgrades"
	}
  }
}
```

#### Mode administrateur

Le mode administrateur fonctionne sur Windows et Linux. Il permet d'éxecuter un plugin avec les droits supplémentaires offerts par le système d'exploitation.
Un utilisateur doit être assigné, car l'execution d'un plugin en mode administrateur doit se faire sans mot de passe, pour des raisons de sécurité.

Il faut être très vigileant en utilisant le mode administrateur, car les plugins font exactement ce qu'on leur demande de faire, et une erreur peut être vite arrivé.

Nous conseillons de l'utiliser en dernier recours et de bien avoir fait les tests nécessaires avant de le mettre en production.

#### Mode differencial

Le mode differencial permet d'économiser de l'espace disque et de l'utilisation réseau.

En effet, pour un plugin, il se peut que son retour soit très souvent le même. Pour éviter, si besoin, de stocker les répétitions, le mode differencial existe.

Prenons l'exemple d'un plugin qui va renvoyer:
	
- l'utilisateur connecté
- l'heure à laquelle le plugin est executé
- le système d'exploitation

Toutes les 5 minutes.

Exemple d'un retour:

```json
{
	"user": "XXX",
	"date": "2020-05-28T14:00:00.000Z",
	"os": "debian"
}
```

Il est alors interessant de n'avoir l'information qu'uniquement quand un nouvel utilisateur est connecté sur la machine.

Dans les attributs du plugin, il faut alors ajouter le mode differencial sur l'attribut "user" et "os".

```json
{
	...
	"plugins": {
		...
		"user_info.sh": {
			"activated": "true",
			"heartbeat": 0.2,
			"differencial": [
				"user",
				"os"
			]
		}
		...
	}
	...
}
```

Si, et uniquement si le résultat des attributs "user" OU "os" sont différents, alors le retour sera sauvegardé.

Maintenant, prenons l'exemple d'un plugin qui va renvoyer la liste auto-générée complète des packages installés sur la machine avec leur version.

Exemple d'un retour:

```json
{
	"package1": {
		"version": "1.0.0",
		"updated": "2020-02-20T14:00:00.000Z"
	},
	"package2": {
		"version": "1.0.1",
		"updated": "2020-01-1T12:00:00.000Z"
	},
	"package3": {
		"version": "4.0.0",
		"updated": "2019-03-02T17:00:00.000Z"
	},
	...
	"package300": {
		"version": "1.2.0",
		"updated": "2018-10-10T18:00:00.000Z"
	}
}
```

Le mode differencial ne suffirait pas, car les entrées sont trop différentes et inconnus d'avance.
Il faudra alors utiliser le mode "differencial_all" qui lui va regarder TOUT les attributs, si le résultat du nouveau est différent du résultat de l'ancien, alors le retour sera sauvegardé.  


```json
{
	...
	"plugins": {
		...
		"packages_list.sh": {
			"activated": "true",
			"heartbeat": 0.2,
			"differencial_all": "true"
		}
		...
	}
	...
}
```

Configuration des clients par le serveur 
-----------------------------------------

Configurer les sauvegardes des résultats
-----------------------------------------

Rédaction plugins
-----------------

Les informations retournées par le plugin doivent être sous un format JSON.
Pour une efficacité optimal, il convient de normaliser les soties de vos plugins. C'est à dire, respecté une convention de nommage définit selon vos principes.

Chaque plugin doit être mit dans le dossier "plugins" du serveur, qui se chargera de les transmettres aux clients. De plus, une entrée doit être écrite dans le fichier config.json pour avoir les informations nécéssaire au bon déroulement du plugin. 

***[Si utilisation de langage de scripts sous Windows](#vialangagenonsupportépardéfaut-et-scripts-windows)***

### Difference entre AssemblyDLL, DLL classique, shared object et script

Le manageur de plugins de HAL permet de lire différents types de DLL pour les plugins, ce qui en fait un outil extrémement modulable.

- Les AssemblyDLL sont les dll générées par la création de bibliothèques .NET, pour Windows et Linux
- Les DLL "classiques" sont celles générées par des compilateurs types C, C++..., mais uniquement pour Windows. 
- Les Shared Objects sont comme les DLL classiques, mais uniquement sous Linux.
- Les scripts peuvent être codé en n'importe quel langage de script (Lua, PHP, Bash, Python, Ruby...). Un fichier source unique doit être fournit par plugin. D'autre interpreteurs peuvent être rajoutés si ceux par défaut ne correspondent pas aux attentes. 

Ces différents format de plugins peuvent être combinés à souhait poour avoir une liste de plugins aussi personalisable que possible.

### Rédaction de plugins via langages supportés par défaut

#### Exemple en C/C++ (DLL classique, Shared Object)

Un point d'entrée est obligatoire pour l'execution du plugin en DLL classique et SO. Le retour de ce point d'entrée sera alors sauvegardé par le client. Il doit impérativement être en JSON.

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
	`gcc -Wall -Wextra -fPIC -shared fichier.c -o ip_infos.dll`

puis copier ip_infos.so dans le dossier plugins et rajouter une entrée dans config.json:

```json
 ...
	"plugins": {
	    ...
		"ip_infos.dll": {
			"activated": "true",
			"heartbeat": "1",
			"os": ["linux"]
		},
	    "ip_infos.so": {
			"activated": "true",
			"heartbeat": "1",
			"os": ["windows"]
		}
		...
	}
 ...
```

#### Exemple en C# (AssemblyDLL)

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

#### Exemple en GO (Script / DLL classique)

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


#### Exemple en Python (Script)

- Créer un fichier `osinfo.py`
- Y ajouter:

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

 - Déplacer le fichier dans le répertoire "plugins" du serveur

### Via langage non supporté par défaut (ET scripts Windows)
#### Ajout d'un interpreteur via fichier de configuration

***Il faut impérativement configurer le chemin des intérpreteurs sur Windows***

Un langage n'est peut être pas supporté par défaut, ou bien l'interpreteur par défaut d'un certain langage ne vous convients pas, il faudra alors pour cela en ajouter un nouveau pour pouvoir executer le plugin.

Il faudra alors modifier le fichier `server/plugins/config.json` pour y ajouter des attributs JSON.

L'attribut `interpreter` possède une liste de système d'exploitation (`windows` et `linux`)

Liste des clé d'intepréteur par défaut:

	- python
	- ruby
	- powershell
	- bash

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

#### Ajout d'une extension de plugin personnalisée

Pour ajouter une extension personnalisée, il suffit d'ajouter l'attribut `custom_extensions` dans le fichier de configuration.

Il faut impérativement que la clé soit l'extension du fichier, et la valeur le nom de l'intérpreteur, qui sera utilisé par l'attribut `interpreter`

```json
{
  "custom_extensions": {
  	".php": "php",
	".lua": "lua"
  },
  "interpreter": {
	"linux": {
	  "php": "path/to/php",
	  "lua": "path/to/lua",
	}
	...
  },

  "plugins": {
	"my_plugin.php": {
		...
	},
	"my_plugin.lua": {
		...
	}
  }
}
```
Ne surtout pas oublier de rajouter un intépréteur, car aucun n'a été défini par défaut, que ce soit via config.json ou les variables d'environnements.

#### Ajout d'in interpreteur via variables d'environnements

Il existe aussi la possibilité de configurer une variable d'environnement (en fonction de votre OS), contenant alors le chemin vers l'intepréteur. Il n'est donc pas obligé de modifier le fichier de configuration avec cette méthode.
La variable d'environnement doit avoir comme clé le nom en majuscule (ex: PYTHON, RUBY, POWERSHELL...) et comme valeur le chemin vers l'intepréteur

Exemple sous Linux:
```
PYTHON=/usr/bin/python3
RUBY=/usr/bin/ruby
BASH=/usr/bin/bash
```

Pour linux, des intepréteurs par défaut sont déjà configurés, il n'est pas alors obligé de les spécifier pour les types de scripts supportés, bien que cela soit très recommandés.


Vérification des sorties des plugins 
------------------------------------
### Présentation du plugins_checker 

Un outil a été crée dans le but de vérifier si une collection de plugins renvoient un json valide.
Pour ça, il faut se rendre dans "plugins_checker" et modifier le fichier "config.json" pour mettre un ou plusieurs chemins de là où se trouve les plugins et le fichier de configuration de ces derniers.
Il faut impérativement suivre ce schéma:

- dossier
  - config.json
  - plugins
    - nomplugin1...
    - nomplugin2...
    - nomplugin3...
...


Le fichier `config.json` est un fichier de configuration normal. Il faut bien mettre `activated` à `true` pour que le plugin soit correctement executé.

Example:
Un dossier "test1" contient:

- config.json
- plugins/
  - cpu_temperature.py
  - kernel_version.sh


et un autre dossier, test2, contient quand à lui:

- config.json
- plugins/
  - connected_user.sh
  - os_informations.rb 


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

Configurer les sauvegardes des résultats
-------------------------------------------

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

  - "text" (sauvegarde sur la sortie console)
  - "local" (sauvegarde sur la machine client)
  - "mangodb" (sauvegarde sur une base mongodb)
  - "influxdb" (sauvegarde sur une base influxdb)
  - "server" (sauvegarde sur le serveur avec des dossiers et fichiers json sous format hiérarchique)

Attention, si vous utilisez `mangodb`, `influxdb` ou tout autre base de donnée, il faut alors généralement spécifier une connection string:

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
	    "mongodb",
	    "influxdb",
	    "server",
	    "mongodb",
		"local"
	],
	"database": {
		"connectionString": [
		    "mongodb://mongodb0.example.com:27017/admin",
		    "http://influxdb.example.com:8086 ",
		    "mongodb://mongodb0.example.com:33333/admin"
		 ]
	},
	...
}
```

Pour rajouter un stockage, il faut alors modifier le code source.

Il faut impérativement créer une classe héritant de IStoragePlugin, et par la suite créer ce dont vous avez besoin.
Ensuite, il faut modifier le fichier: client/factory/StorageFactory.cs pour y rajouter votre stockage personnalisé. Toutes les informations de comment procéder sont mit en commentaires dans ce fichier.