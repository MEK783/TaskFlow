
import React from 'react'

export default function Watermark(){
  return (
    <img
      src="/watermark.svg"
      alt="MEK watermark"
      className="fixed bottom-6 right-6 w-64 rotate-15 opacity-10 dark:opacity-20 pointer-events-none select-none"
    />
  )
}
