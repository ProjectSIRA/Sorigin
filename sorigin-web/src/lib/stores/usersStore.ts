import type AuthedUser from '$lib/types/authedUser'
import { writable } from 'svelte/store'

export const authedUser = writable(getUser())

function getUser(): AuthedUser | null {
    return null
}