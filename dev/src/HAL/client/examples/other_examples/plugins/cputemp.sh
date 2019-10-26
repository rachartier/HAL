get_cpu_temperature() {
    temp=$( cat /sys/devices/virtual/thermal/thermal_zone0/temp 2> /dev/null )

	[[ -z $temp ]] && echo "0" && return

    temp=`expr $temp / 1000`
    echo $temp
}


echo "{\"cpu_temp\":$(get_cpu_temperature)}";
