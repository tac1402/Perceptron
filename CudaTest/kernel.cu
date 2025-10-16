
#include "cuda_runtime.h"
#include "device_launch_parameters.h"

#include <stdio.h>
#include <iostream>
#include <windows.h>

int main() {

    // Устанавливаем кодировку консоли на UTF-8
    SetConsoleOutputCP(CP_UTF8);
    SetConsoleCP(CP_UTF8);

    int deviceCount;
    cudaGetDeviceCount(&deviceCount); // Получаем количество доступных GPU

    for (int i = 0; i < deviceCount; i++) {
        cudaDeviceProp devProp;
        cudaGetDeviceProperties(&devProp, i); // Получаем свойства устройства

        std::cout << "GPU device " << i << ": " << devProp.name << std::endl;
        std::cout << "==========================================" << std::endl;

        // 1. Ключевые ограничения и архитектура
        std::cout << "1. Ключевые ограничения и архитектура:" << std::endl;
        std::cout << "  - Compute Capability: " << devProp.major << "." << devProp.minor << std::endl;
        std::cout << "  - Количество Streaming Multiprocessors (SM): " << devProp.multiProcessorCount << std::endl;

        // 2. Ограничения на количество потоков
        std::cout << "2. Ограничения на количество потоков:" << std::endl;
        std::cout << "  - Максимальное количество потоков на блок: " << devProp.maxThreadsPerBlock << std::endl;
        std::cout << "  - Максимальные размеры блока (x, y, z): "
            << "(" << devProp.maxThreadsDim[0] << ", "
            << devProp.maxThreadsDim[1] << ", "
            << devProp.maxThreadsDim[2] << ")" << std::endl;
        std::cout << "  - Максимальные размеры сетки (x, y, z): "
            << "(" << devProp.maxGridSize[0] << ", "
            << devProp.maxGridSize[1] << ", "
            << devProp.maxGridSize[2] << ")" << std::endl;

        // 3. Параметры производительности
        std::cout << "3. Параметры производительности:" << std::endl;
        std::cout << "  - Размер warp: 32 threads" << std::endl; // Warp size обычно 32
        std::cout << "  - Максимальное количество варпов на SM: " << devProp.maxThreadsPerMultiProcessor / 32 << std::endl;
        std::cout << "  - Максимальное количество потоков на SM: " << devProp.maxThreadsPerMultiProcessor << std::endl;
        std::cout << "  - Общий объем shared memory на блок: " << devProp.sharedMemPerBlock / 1024 << " KB" << std::endl;

        // 4. Аппаратные ресурсы
        std::cout << "4. Аппаратные ресурсы:" << std::endl;
        std::cout << "  - Объем глобальной памяти: " << devProp.totalGlobalMem / (1024 * 1024) << " MB" << std::endl;
        std::cout << "  - Размер кэша L2: " << devProp.l2CacheSize / 1024 << " KB" << std::endl;

        std::cout << std::endl;
    }

    return 0;
}