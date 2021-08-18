import type AuthedUser from '$lib/types/authedUser'
import cookie from 'cookie'

export async function handle({ request, resolve }) {
    const cookies = cookie.parse(request.headers.cookie || '')
    if (cookies.user !== undefined && cookies.user !== null) {
        request.locals.user = JSON.parse(cookies.user) as AuthedUser
    }
    const response = await resolve(request)

    if (request.locals.user === 'delete') {
        response.headers['set-cookie'] = `user=logout; path=/; expires=Thu, 01 Jan 1970 00:00:00 GMT`
    }
    else if (request.locals.user !== undefined && cookies.user === undefined) {
        let fourHours = new Date()
        fourHours.setTime(fourHours.getTime() + 60 * 1000)
        response.headers['set-cookie'] = `user=${JSON.stringify(request.locals.user) || ''}; expires=${fourHours.toUTCString()}; Path=/; HttpOnly`
    }

    return response
}

export async function getSession(request) {
    return {
        user: request.locals.user
    }
}