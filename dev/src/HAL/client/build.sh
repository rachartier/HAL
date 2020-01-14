#!/bin/sh

BUILDPATH=$(pwd)/bin/Release/netcoreapp3.0/linux-x64/publish

dotnet publish -c Release /p:DefineConstants=PRODUCTION_BUILD /p:PublishSingleFile=true /p:Mode=CoreRT-Moderate /p:PublishSingleFile=true /p:PublishTrimmed=true --self-contained true

if [ ! -d $BUILDPATH/plugins ]; then
    mkdir $BUILDPATH/plugins
    echo "\033[0;32mAll plugins copied."
fi

cp -rf config $BUILDPATH
echo "\033[0;32mConfiguration copied."

if [ ! -f $BUILDPATH/config/config_local.json ]; then
    echo "{\n}" > $BUILDPATH/config/config_local.json
    echo "\033[0;32mEmpty config/config_local.json created. You need to put server configuration inside."
fi

cp -rf nlog.config $BUILDPATH
echo "\033[0;32mnlog.config copied."

echo "\033[0;32mProduction build successful."
