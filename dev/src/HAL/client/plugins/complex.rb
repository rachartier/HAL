require 'json'

class Coord
  attr_reader :x, :y, :z
  def initialize(x,y,z)
    @x = x
    @y = y
    @z = z
  end
  
  def as_json(option={})
    {
      x: @x,
      y: @y,
      z: @z,
    }
  end

  def to_json(*options)
    as_json(*options).to_json(*options)
  end
end

class Cube
  attr_reader :name, :point

  def initialize 
    @point = Coord.new(0,0,0)
    @name = "cube"
  end

  def as_json(option={})
    {
      name: @name,
      point: @point
    }
  end

  def to_json(*options)
    as_json(*options).to_json(*options)
  end
end

cube = Cube.new

print cube.to_json
