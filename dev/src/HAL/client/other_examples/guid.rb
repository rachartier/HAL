require 'securerandom'
require 'json'

puts ({guid: SecureRandom.uuid}.to_json)
