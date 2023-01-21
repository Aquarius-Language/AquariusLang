Stuffs implemented from "Writing an Interpreter in Go" book:
    ~ page 175.

Statement and expression nodes under AbstractSyntaxTree.cs needs to be class. 
Because they implement from IStatement and IExpression interfaces. Structs are
bad for type casting (polymorphism).

To implement:

1. For loop, while loop.
2. Floating point numbers.
3. Variable re-assignment.
4. Use "Visitor pattern" to replace "type checking using switch case" under Evaluator.Eval().
