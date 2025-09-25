#include "mimalloc-new-delete.h"
#include "lib.h"

#include "Face.h"

using namespace Coplt;

IFace* LibTextLayout::Impl_CreateFace()
{
    return new Face();
}

ILibTextLayout* Coplt::Coplt_CreateLibTextLayout()
{
    return new LibTextLayout();
}
