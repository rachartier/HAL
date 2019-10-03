echo "not admin"
if [ "$EUID" -ne 0 ]
  then echo "please run as root"
  exit
fi

echo "admin"
