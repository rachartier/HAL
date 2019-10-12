io.write("{\"random_numbers\":[")

for i=1, 10 do
	if i == 10 then
		io.write(math.random(100))	
	else 
		io.write(math.random(100) .. ",")	
	end
end

io.write("]}\n")
