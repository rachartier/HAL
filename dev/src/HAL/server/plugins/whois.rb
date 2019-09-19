require 'etc'
require 'json'

login = Etc.getlogin()

puts( { login: login }.to_json )
