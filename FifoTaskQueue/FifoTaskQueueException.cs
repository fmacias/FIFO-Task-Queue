/**
 * LICENSE
 *
 * This source file is subject to the new BSD license that is bundled
 * with this package in the file LICENSE.txt.
 *
 * @copyright   Copyright (c) 2021. Fernando Macias Ruano.
 * @E-Mail      fmaciasruano@gmail.com > .
 * @license    https://github.com/fmacias/Scheduler/blob/master/Licence.txt
 */
using System;

namespace fmacias
{
    class FifoTaskQueueException:Exception
    {
        public FifoTaskQueueException()
        {
        }

        public FifoTaskQueueException(string message)
            : base(message)
        {
        }

        public FifoTaskQueueException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
