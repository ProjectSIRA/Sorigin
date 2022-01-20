import { browser } from '$app/env';
import type User from '$lib/types/User';
import { REFETCH_URL, SORIGIN_URL } from '$lib/utils/env';
import axios, { AxiosError } from 'axios';

export interface UserResponse {
	user: User | undefined;
	error: string | null;
	errorMessage: string | null;
}

export function soriginURL() {
	return browser ? SORIGIN_URL : REFETCH_URL;
}

export async function userByID(id: number): Promise<UserResponse> {
	try {
		const response = await axios.get(soriginURL() + '/api/users/' + id);
		const user = response.data as User;
		return { user, error: null, errorMessage: null } as UserResponse;
	} catch (err) {
		console.log(err);
		if (axios.isAxiosError(err)) {
			const error = err as AxiosError;
			if (error.response?.data?.error && error.response?.data?.errorMessage) {
				return {
					user: null,
					error: error.response?.data?.error,
					errorMessage: error.response?.data?.errorMessage
				} as UserResponse;
			}
		}
	}
	return { user: null, error: 'unknown-error', errorMessage: 'An unknown error has occured.' };
}
