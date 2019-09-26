#!/usr/bin/perl

sub timestamp {
  return localtime (time);
}

print 'script perl: ' . timestamp() . "\n";

