import { createSWR } from 'sswr'

const swr = createSWR({
    fetcher: (key) => fetch(key, {
        headers: {
            Accept: 'application/json',
            'Content-Type': 'application/json',
        }
    }).then((res) => res.json()),
})

export const useSWR = swr.useSWR
export const mutate = swr.mutate
export const revalidate = swr.revalidate
export const clear = swr.clear