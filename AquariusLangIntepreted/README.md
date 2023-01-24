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

1. While loop.
2. Floating point numbers.
3. Use "Visitor pattern" to replace "type checking using switch case" under Evaluator.Eval(). (for performance enhancement)
4. Make numbers available as part of identifiers(variable names).
5. Implement else if (elif)...
6. Ignore comments.
7. Value re-assignment to array and hashmap; pass them by reference to functions.
8. "break" from loops.
9. Implement "outer" and "local" variables for environments. (to fix local variable problems in loop)
10. Add "for loop example unit testing" when the problems for for loop are fixed.
11. Make for loop still work even when no few statements in parenthesis. ex. for(; i < 10; i+=1){}
12. Binary and, or (|, &)
13. String concatenate with int.

Finished implementing:

1. Variable re-assignment. Added to Evaluator.assignVariableVal().
2. For loop. Added to Evaluator.evalForLoopLiteral() and.parseForLoopLiteral().  (global and local issues not completely done yet)
3. ~~String keys are saved as hashcode/int (.GetHashCode()) in dictionary for Hashmap type in Aquarius lang.
   The hashcode version of the string and int values might happen to have collisions of same value. To fix
   that, can use linked lists as values for the hashmap. (learnt from Algorithm course in university)~~
   Turns out there's no need to do that, since HashKey instance contains "type" member as string. Therefore,
   the hashmap in the language can differentiate between two instances with different types but same hash int values.
4. Implement ||, &&, <=, >=...
5. "return" out of function from loop.

SUGGESTIONS AND NOTES WHEN IMPLEMENTING NEW FEATURES:

- Be VERY CAREFUL about when to call nextTokens(). I missed few nextTokens() call when I was implementing for loop parsing.
  But they seem to get fixed when I added those function calls.
- Sometimes when adding new operators but it's not showing up in search for prefix/infix callbacks, make sure if they're being 
  referenced under Parser.precedencesMap.