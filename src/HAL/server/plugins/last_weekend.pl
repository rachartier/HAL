use POSIX qw(strftime);

$today = date(time);
$weekend = date2(time);

# start of subroutine date MM-DD-YYYY
sub date 
{
	my($time) = @_;     # incoming parameters

	@when = localtime($time);
	$dow=$when[6];
	$when[5]+=1900;
	$when[4]++;
	$date = $when[5] . "-" . $when[4] . "-" . $when[3] . "T00:00:00";

	return $date;
}

# start of subroutine date2 MM-DD-YYYY
sub date2
{
	my($time) = @_;     # incoming parameters

	$offset = 0;
	$offset = 60*60*24*$dow;
	@when = localtime($time - $offset);
	$when[5]+=1900;
	$when[4]++;
	$date = $when[5] . "-" . $when[4] . "-" . $when[3] . "T00:00:00";

	return $date;
}

print "{\"last_weekend\": \"", $weekend, "\"}\n";
