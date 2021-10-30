import type UserSession from '$lib/models/UserSession';
import { writable } from 'svelte/store';

export const sessionStore = writable<UserSession | null>(null);
