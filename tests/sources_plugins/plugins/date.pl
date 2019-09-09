use strict;
use warnings;

use POSIX qw(strftime);

my $dt = time();

print "{\"date\": \"", strftime('%Y-%m-%dT%H:%M:%S', gmtime($dt)), "\"}\n";
