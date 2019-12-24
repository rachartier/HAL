listUpgrades=`apt-get -s dist-upgrade | awk '/^Inst/ { print $2 }'`
needUpgrade="true"

[[ -z "$listUpgrades" ]] && needUpgrade="false" # thats ugly  

echo -n "{\"need_upgrades\": \"$needUpgrade\"}"
