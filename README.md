# ExpressWalker
ExpressWalker provides a generic way to examine and change any object graph in fashion similar to "Visitor Pattern". You can build generic hierarchy composition (visitor) capable to "visit" and change any object's property, basing on configuration. Relies purely on expression trees while visiting objects (uses reflection only once while building a visitor).

That's why **IT IS WAY FASTER** than custom solutions usually built with reflection.

It is optionally protected from circular references so you can avoid StackOverflowException easily.
Provides fluent API while building a visitor which increases code readability 
in terms of recognizing the hierarchy being built right away from the code.
The optional and configurable things available are:

- visiting properties by matching owner type and property name 
- visiting properties by matching property type only
- specifying depth of visit in run-time (not during configuration)
- custom expression for changing property value 
- cloning of visited object
- visiting through items in colleciton properties
- etc.

```

//example 1 - IVisitor that visits properties by property names and/or types (start from TypeWalker class):

    var typeVisitor = TypeWalker<Parent>.Create()
						.ForProperty<Parent, string>(p => p.TestString1, (old, met) => old + met)
						.ForProperty<Child, DateTime>(p => p.TestDate1, (old, met) => old.AddYears(10))
						.ForProperty<CommonType>((old, met) => new CommonType { CommonString = "..." })
					.Build(depth:10, guard:new PropertyGuard(), supportsCloning: true);
					
	//guard is protection against type-wise circular references. supportsCloning = false improves build time.

    var parentClone = new Parent();
    var propertyValues = new HashSet<PropertyValue>()
    typeVisitor.Visit(parentObject, parentClone, parentObject, parentClone, depth:10, guard:new InstanceGuard(), values:propertyValues); 
  
//example 2 - IVisitor that visits properties by explicit configuration (start from ManualWalker class):

    var manualVisitor = ManualWalker.Create<A1>()
                                    .Property<A1, DateTime>(a1 => a1.A1Date, (va1, met) => va1.AddYears(10))
                                    .Element<A1, B1>(a1 => a1.B1, b1 =>
                                            b1.Property<B1, string>(x => x.B1Name, (vb1, met) => vb1 + "Test2"))
                                    .Collection<A1, B2>(a1 => a1.B2List, b2 => b2
                                            .Property<B2, DateTime>(x => x.B2Date, (vb2, met) => vb2.AddYears(10)))
                                .Build();

    var parentClone = new A1();
    var propertyValues = new HashSet<PropertyValue>()
    manualVisitor.Visit(parentObject, parentClone, parentObject, parentClone, depth:10, guard:new InstanceGuard(), values:propertyValues);
			
//Paremeter 'met' in expressions above is optional metadata object set in design-time. 
//It can be set by [VisitorMetadata] property attribute in visited class.
//e.g. in example above, there is [VisitorMetadata("AnyString")] on property Parent.TestString1.

//example 3 - IVisitor built and cached using the IVisitorsFactory:
//scenario for visitors of same settings built for different types:
  
  var factory = new VisitorsFactory().WithSettings("name1", depth:5, usePropertyGuard:false, supportsCloning:false)
                                       .ForProperty<int>((val, met) => 2)
                                     .WithSettings("name6")
                                       .ForProperty<Parent, string>( x => x.Name, (val, met) => "t");
									 
  var visitor1 = factory.GetVisitor("name1", typeof(Class1));
  var visitor1a = factory.GetVisitor("name1", typeof(Class1));
  var visitor2 = factory.GetVisitor("name1", typeof(Class2));
  var visitor6 = factory.GetVisitor("name6", typeof(Class6));
  
//visitor1 == visitor1a --true
//visitor1 == visitor2 --false	 
			
```

Many thanks to Francisco José Rey Gozalo for contributing with ideas and solutions.