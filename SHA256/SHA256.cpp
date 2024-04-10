#include "pch.h"
#include "SHA256.h"

#include <iostream>
#include <fstream>
#include <vector>
#include <bitset>
#include <iomanip>
#include <sstream>

uint32_t H[8] = { 0x6a09e667, 0xbb67ae85, 0x3c6ef372, 0xa54ff53a, 0x510e527f, 0x9b05688c, 0x1f83d9ab, 0x5be0cd19 };
const uint32_t k[64] = {
    0x428a2f98, 0x71374491, 0xb5c0fbcf, 0xe9b5dba5, 0x3956c25b, 0x59f111f1, 0x923f82a4, 0xab1c5ed5,
    0xd807aa98, 0x12835b01, 0x243185be, 0x550c7dc3, 0x72be5d74, 0x80deb1fe, 0x9bdc06a7, 0xc19bf174,
    0xe49b69c1, 0xefbe4786, 0x0fc19dc6, 0x240ca1cc, 0x2de92c6f, 0x4a7484aa, 0x5cb0a9dc, 0x76f988da,
    0x983e5152, 0xa831c66d, 0xb00327c8, 0xbf597fc7, 0xc6e00bf3, 0xd5a79147, 0x06ca6351, 0x14292967,
    0x27b70a85, 0x2e1b2138, 0x4d2c6dfc, 0x53380d13, 0x650a7354, 0x766a0abb, 0x81c2c92e, 0x92722c85,
    0xa2bfe8a1, 0xa81a664b, 0xc24b8b70, 0xc76c51a3, 0xd192e819, 0xd6990624, 0xf40e3585, 0x106aa070,
    0x19a4c116, 0x1e376c08, 0x2748774c, 0x34b0bcb5, 0x391c0cb3, 0x4ed8aa4a, 0x5b9cca4f, 0x682e6ff3,
    0x748f82ee, 0x78a5636f, 0x84c87814, 0x8cc70208, 0x90befffa, 0xa4506ceb, 0xbef9a3f7, 0xc67178f2,
};

uint32_t r_rotate(uint32_t x, uint32_t n) {
    return (x >> n) | (x << (32 - n));
}

uint32_t sigma0(uint32_t x) {
    return r_rotate(x, 7) ^ r_rotate(x, 18) ^ (x >> 3);
}

uint32_t sigma1(uint32_t x) {
    return r_rotate(x, 17) ^ r_rotate(x, 19) ^ (x >> 10);
}

uint32_t Sigma0(uint32_t x) {
    return r_rotate(x, 2) ^ r_rotate(x, 13) ^ r_rotate(x, 22);
}

uint32_t Sigma1(uint32_t x) {
    return r_rotate(x, 6) ^ r_rotate(x, 11) ^ r_rotate(x, 25);
}

uint32_t choice(uint32_t x, uint32_t y, uint32_t z) {
    return (x & y) ^ (~x & z);
}

uint32_t majority(uint32_t a, uint32_t b, uint32_t c) {
    return (a & (b | c)) | (b & c);
}

void preprocessFile(std::ifstream& file, std::vector<std::bitset<8>>& bytes) {
    char byte;
    int bitCount = 0;
    int totalBits = 512;

    while (file.get(byte)) {
        bytes.push_back(std::bitset<8>(byte));
        bitCount += 8;

        if ((totalBits - bitCount) < 72) {
            totalBits += 512;
        }
    }

    int extraBits = (totalBits - bitCount) - 8;
    bytes.push_back(std::bitset<8>("10000000"));

    for (int i = extraBits; i > 64; i -= 8) {
        bytes.push_back(std::bitset<8>("00000000"));
    }

    std::string length = std::bitset<64>(bitCount).to_string();
    for (int i = 0; i < 8; i++) {
        bytes.push_back(std::bitset<8>(length.substr(i * 8, 8)));
    }
}

void scheduleWords(std::vector<std::bitset<8>>& bytes, int& block, std::vector<uint32_t>& words) {
    std::string tempstring = "";
    for (int i = (block * 64) - 64; i < block * 64; i++) {
        tempstring.append(bytes.at(i).to_string());

        if (tempstring.length() == 32) {
            words.push_back(std::bitset<32>(tempstring).to_ulong());
            tempstring = "";
        }
    }

    for (int i = words.size(); i < 64; i++) {
        int size = words.size();

        words.push_back(
            sigma1(words.at(size - 2)) + words.at(size - 7) + sigma0(words.at(size - 15)) + words.at(size - 16)
        );
    }
}

void processBlocks(std::vector<std::bitset<8>>& bytes, int& block) {
    std::vector<uint32_t> words;
    scheduleWords(bytes, block, words);

    //                      A     B     C     D    E     F      G     H
    uint32_t compr[8] = { H[0], H[1], H[2], H[3], H[4], H[5], H[6], H[7] };

    for (int i = 0; i < words.size(); i++) {
        uint32_t T1 = Sigma1(compr[4]) + choice(compr[4], compr[5], compr[6]) + compr[7] + k[i] + words[i];
        uint32_t T2 = Sigma0(compr[0]) + majority(compr[0], compr[1], compr[2]);

        for (int a = 7; a >= 0; a--) {
            if (a == 0) {
                compr[a] = T1 + T2;
            }
            else {
                compr[a] = compr[a - 1];
            }
        }

        compr[4] += T1;
    }

    for (int i = 0; i < 8; i++) {
        H[i] += compr[i];
    }
}

std::string ToHex(uint32_t n) {
    std::stringstream stream;
    stream << std::setfill('0') << std::setw(8) << std::hex << n;
    return stream.str();
}

void convert(std::ifstream& file, std::string& hex) {

    std::vector<std::bitset<8>> bytes;
    preprocessFile(file, bytes);

    for (int i = 1; i <= (bytes.size() * 8) / 512; i++) {
        processBlocks(bytes, i);
    }

    for (int i = 0; i < 8; i++) {
        hex.append(ToHex(H[i]));
    }

    H[0] = 0x6a09e667;
    H[1] = 0xbb67ae85;
    H[2] = 0x3c6ef372;
    H[3] = 0xa54ff53a;
    H[4] = 0x510e527f;
    H[5] = 0x9b05688c;
    H[6] = 0x1f83d9ab;
    H[7] = 0x5be0cd19;

    file.close();
}

SHA256_API void __cdecl ToSHA256(const char* filePath, char* buf)
{
    std::string filename(filePath);
    std::ifstream file(filename, std::ios::binary);

    if (!file.is_open()) {
        std::cerr << "Failed to open file: " << filename << std::endl;
        return;
    }

    std::string hex = "";
    convert(file, hex);

    strcpy_s(buf, hex.size()+1, hex.c_str());
}
