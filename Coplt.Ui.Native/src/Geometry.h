#pragma once

namespace Coplt::Geometry
{
    template <class T>
    struct Size
    {
        T Width;
        T Height;
    };

    template <class T>
    struct Point
    {
        T X;
        T Y;
    };
}
