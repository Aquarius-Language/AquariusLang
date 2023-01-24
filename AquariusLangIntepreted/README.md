Stuffs implemented from "Writing an Interpreter in Go" book:
    ~ page 189.

IMPORTANT!:
- Statement and expression nodes under AbstractSyntaxTree.cs needs to be class. 
Because they implement from IStatement and IExpression interfaces. Structs are
bad for type casting (polymorphism).
- HashKey MUST either be struct type for being stack variables, or they need to override Equals() and GetHashCode(),
so they can be used as dictionary keys. Otherwise, it'll not work as dictionary keys. (Maybe because class instances are pointers to heap?)
Extra advantage of this: since HashKey doesn't have complicated types nor polymorphic members, it also might boost performance.

To implement:

1. For loop, while loop.
2. Floating point numbers.
3Use "Visitor pattern" to replace "type checking using switch case" under Evaluator.Eval().
3. ~~String keys are saved as hashcode/int (.GetHashCode()) in dictionary for Hashmap type in Aquarius lang.
   The hashcode version of the string and int values might happen to have collisions of same value. To fix
   that, can use linked lists as values for the hashmap. (learnt from Algorithm course in university)~~
   Turns out there's no need to do that, since HashKey instance contains "type" member as string. Therefore,
   the hashmap in the language can differentiate between two instances with different types but same hash int values.
4. Make numbers available as part of identifiers(variable names).
5. Implement ||, &&, <=, >=...
6. Implement else if (elif)...
7. Ignore comments.

Finished implementing:

1. Variable re-assignment. (Implemented. Added to Evaluator.assignVariableVal().)