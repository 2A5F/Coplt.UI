#pragma once

#ifndef COPLT_EXPORT
#ifdef _MSC_VER
#define COPLT_EXPORT __declspec(dllexport)
#else
#define COPLT_EXPORT
#endif
#endif
