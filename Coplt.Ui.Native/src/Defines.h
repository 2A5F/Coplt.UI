#pragma once

#ifndef COPLT_EXPORT
#if defined(_MSC_VER) && defined(COPLT_SOURCE)
#define COPLT_EXPORT __declspec(dllexport)
#else
#define COPLT_EXPORT
#endif
#endif
