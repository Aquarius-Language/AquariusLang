Stuffs implemented from "Writing an Interpreter in Go" book:
    ~ page 170.

Statement and expression nodes under AbstractSyntaxTree.cs needs to be class. 
Because they implement from IStatement and IExpression interfaces. Structs are
bad for type casting (polymorphism).
