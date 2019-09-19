ping=`ping -c 1 google.com | tail -1 | awk '{print $4}' | cut -d '/' -f 1)`
echo "{\"ping\": $ping}"
