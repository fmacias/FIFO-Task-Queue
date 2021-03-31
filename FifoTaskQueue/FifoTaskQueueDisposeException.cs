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
    class FifoTaskQueueDisposeException:Exception
    {
        public FifoTaskQueueDisposeException()
        {
        }

        public FifoTaskQueueDisposeException(string message)
            : base(message)
        {
        }

        public FifoTaskQueueDisposeException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
