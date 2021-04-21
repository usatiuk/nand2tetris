// This file is part of www.nand2tetris.org
// and the book "The Elements of Computing Systems"
// by Nisan and Schocken, MIT Press.
// File name: projects/04/Fill.asm

// Runs an infinite loop that listens to the keyboard input.
// When a key is pressed (any key), the program blackens the screen,
// i.e. writes "black" in every pixel;
// the screen should remain fully black as long as the key is pressed. 
// When no key is pressed, the program clears the screen, i.e. writes
// "white" in every pixel;
// the screen should remain fully clear as long as no key is pressed.

// Put your code here.

@SCREEN
D=A
@PTR
M=D

(WHITE)
@KBD
D=M
@BLACK
D;JNE

@SCREEN
D=A
@PTR
M=D

(WLOOP)
@PTR
D=M
@R0
M=D
@KBD
D=A
@R0
D=D-M
@WHITE
D;JEQ

@PTR
A=M
M=0

@PTR
M=M+1

@WLOOP
0;JMP

(BLACK)
@KBD
D=M
@WHITE
D;JEQ

@SCREEN
D=A
@PTR
M=D

(BLOOP)
@PTR
D=M
@R0
M=D
@KBD
D=A
@R0
D=D-M
@BLACK
D;JEQ

@PTR
A=M
M=-1

@PTR
M=M+1

@BLOOP
0;JMP
