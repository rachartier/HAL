require 'json'
require 'time'

puts JSON.generate(
  {
    :date => Time.now.utc.iso8601
  }
)
