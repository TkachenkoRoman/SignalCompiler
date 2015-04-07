.386
.MODEL	small
.STACK	256
.DATA
FIRST	dd	?
SECOND	dd	?
THIRD	dd	?
.CODE
HELLOWORLD	PROC
mov	eax, 1
mov	ebx, FIRST
cmp	eax, ebx
jne	L1
mov	eax, 5
mov	ebx, 6
cmp	eax, ebx
jne	L2
jmp	L3 
L2:
mov	eax, 4
mov	ebx, 5
cmp	eax, ebx
jne	L4
L4:
L3:
jmp	L5 
L1:
mov	eax, SECOND
mov	ebx, 1
cmp	eax, ebx
jne	L6
L6:
L5:
mov	ah,4Ch
mov	al,0
int	21h
HELLOWORLD	ENDP
END	HELLOWORLD