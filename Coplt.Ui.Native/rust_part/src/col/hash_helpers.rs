pub const HASH_COLLISION_THRESHOLD: u32 = 100;
pub const MAX_PRIME_ARRAY_LENGTH: i32 = 0x7FFFFFC3;
pub const HASH_PRIME: i32 = 101;

pub const PRIMES: &[i32; 72] = &[
    3i32, 7, 11, 17, 23, 29, 37, 47, 59, 71, 89, 107, 131, 163, 197, 239, 293, 353, 431, 521, 631,
    761, 919, 1103, 1327, 1597, 1931, 2333, 2801, 3371, 4049, 4861, 5839, 7013, 8419, 10103, 12143,
    14591, 17519, 21023, 25229, 30293, 36353, 43627, 52361, 62851, 75431, 90523, 108631, 130363,
    156437, 187751, 225307, 270371, 324449, 389357, 467237, 560689, 672827, 807403, 968897,
    1162687, 1395263, 1674319, 2009191, 2411033, 2893249, 3471899, 4166287, 4999559, 5999471,
    7199369,
];

pub fn is_prime(candidate: i32) -> bool {
    if (candidate & 1) != 0 {
        let limit = candidate.isqrt();
        for divisor in (3..=limit).step_by(2) {
            if (candidate % divisor) == 0 {
                return false;
            }
        }
        return true;
    }
    candidate == 2
}

pub fn get_prime(min: i32) -> i32 {
    for &prime in PRIMES {
        if prime >= min {
            return prime;
        }
    }

    // Outside of our predefined table. Compute the hard way.
    for i in ((min | 1)..=i32::MAX).step_by(2) {
        if is_prime(i) && ((i - 1) % HASH_PRIME != 0) {
            return i;
        }
    }

    min
}

pub fn get_fast_mod_multiplier(divisor: u32) -> u64 {
    u64::MAX / (divisor as u64) + 1
}

#[inline]
pub fn fast_mod(value: u32, divisor: u32, multiplier: u64) -> u32 {
    debug_assert!(divisor <= i32::MAX as u32);

    let highbits = (((((multiplier * value as u64) >> 32) + 1) * divisor as u64) >> 32) as u32;

    debug_assert!(highbits == value % divisor);

    highbits
}

pub fn expand_prime(old_size: i32) -> i32 {
    let new_size = 2 * old_size;

    if new_size as u32 > MAX_PRIME_ARRAY_LENGTH as u32 && MAX_PRIME_ARRAY_LENGTH > old_size {
        debug_assert!(
            MAX_PRIME_ARRAY_LENGTH == get_prime(MAX_PRIME_ARRAY_LENGTH),
            "Invalid MaxPrimeArrayLength"
        );

        return MAX_PRIME_ARRAY_LENGTH;
    }

    get_prime(new_size)
}
