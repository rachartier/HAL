for i in `seq 1 100`
do
	file="plugins/file_stress_$i.py"
	dd if="/dev/urandom" bs=1024 count=64 | base64 > "$file" 
done

echo "stress plugins added."
