import type User from './user'

export default interface AuthedUser {
    refresh: string,
    token: string,
    user: User,
}