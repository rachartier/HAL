#ifndef __DEBUG_H__
#define __DEBUG_H__

#ifdef DEBUG
# define DPRINT(msg, ...) printf("line %d: " msg "\n", __LINE__, __VA_ARGS__ )
#else 
# define DPRINT(msg, ...) 
#endif

#endif // __DEBUG_H__