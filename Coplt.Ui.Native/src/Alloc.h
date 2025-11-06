#pragma once

#include "Defines.h"

extern "C" COPLT_EXPORT void* coplt_ui_malloc(const size_t size, const size_t align);

extern "C" COPLT_EXPORT void coplt_ui_free(void* ptr, const size_t align);

extern "C" COPLT_EXPORT void* coplt_ui_zalloc(const size_t size, const size_t align);

extern "C" COPLT_EXPORT void* coplt_ui_realloc(void* ptr, const size_t new_size, const size_t align);
