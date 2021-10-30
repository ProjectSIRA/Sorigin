import { browser } from '$app/env';
import { get } from '$lib/sorFetcher';
import type User from '$lib/types/user';
import { REFETCH_URL, SORIGIN_URL } from '$lib/utils/env';

export interface UserResponse {
    user: User | null;
    error: string | null;
}

export function soriginURL() {
    return browser ? SORIGIN_URL : REFETCH_URL;
}

export function userByUsername(name: string) {
    const url = `/user/by-username/${name.toLowerCase()}`;
    return get<User>(url);
}

export async function getUsernames() {
    const usernameListURL = `/user`;
    const userRes = await get<string[]>(usernameListURL);

    if (userRes.data) {
        return userRes.data;
    }
    return [] as string[];
}
