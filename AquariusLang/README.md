Statement and expression nodes under AbstractSyntaxTree.cs needs to be class. 
Because they implement from IStatement and IExpression interfaces. Structs are
bad for type casting (polymorphism).