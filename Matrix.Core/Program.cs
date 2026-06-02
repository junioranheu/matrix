var humanFactory = new HumanFactory();

var worldFactory = new WorldFactory(humanFactory);

var world = worldFactory.Create();